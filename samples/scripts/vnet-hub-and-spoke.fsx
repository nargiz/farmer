#r @"..\..\src\Farmer\bin\Debug\net5.0\Farmer.dll"

open Farmer
open Farmer.Builders
open Farmer.Arm.Network
open Farmer.WebApp


let hub =
    vnet {
        name "vnet-hub"

        build_address_spaces [ addressSpace {
                                   space "10.0.0.0/16"

                                   subnets [ subnetSpec {
                                                 name "management"
                                                 size 24
                                             }
                                             subnetSpec {
                                                 name "workload"
                                                 size 18
                                                 allow_private_endpoints Enabled
                                             }
                                             subnetSpec {
                                                 name "AzureBastionSubnet"
                                                 size 24
                                             }
                                             subnetSpec {
                                                 name "GatewaySubnet"
                                                 size 24
                                             } ]
                               } ]
    }

// let gateway =
//     gateway {
//         name "vnet-gateway"
//         vnet hub
//     }

let spokes =
    // let hubPeering = vnetPeering{
    //     remote_vnet hub
    //     transit UseRemoteGateway
    //     depends_on gateway
    // }
    let spoke i vnetName =
        vnet {
            name $"vnet-%s{vnetName}"

            build_address_spaces [ addressSpace {
                                       space $"10.%i{i}.0.0/16"

                                       subnets [ subnetSpec {
                                                     name "management"
                                                     size 24
                                                 }
                                                 subnetSpec {
                                                     name "endpoints"
                                                     size 24
                                                     allow_private_endpoints Enabled
                                                 }
                                                 subnetSpec {
                                                     name "serverFarms"
                                                     size 18
                                                 } ]
                                   } ]

            //add_peering hubPeering
        }

    {| BuildAgents = spoke 1 "buildagents"
       InternalServices = spoke 2 "internal"
       PublicServices = spoke 3 "public" |}

let jumpBoxes =
    [ hub
      spokes.InternalServices
      spokes.PublicServices ]
    |> List.map
        (fun vnet ->
            vm {
                name (vnet.Name.Value.Replace("vnet-", "vm-"))
                link_to_vnet vnet
                subnet_name "management"
                username "testadmin"
                password_parameter "jump-password"
                public_ip None
            }
            :> IBuilder)

let bastion =
    bastion {
        name "bastion"
        vnet hub.Name.Value
    }

// TODO: Azure Firewall, Azure Application Gateway, Route Tables, NSGs


let integratedApp  = 
    webApp{
        name "codat-RSP-test-app"
        sku Sku.P1V2
        vnet_integrate (spokes.InternalServices,"serverFarms")
        add_private_endpoint (spokes.InternalServices,"endpoints")
    }
arm {
    add_resources [ hub
                    //spokes.BuildAgents
                    spokes.InternalServices ]
                    //spokes.PublicServices ]

    //add_resources jumpBoxes

    //add_resource gateway
    add_resource integratedApp
    //add_resource bastion
}
|> Deploy.execute "RSP-test-2" [  ]
//|> Writer.quickWrite "hub-and-spoke"
