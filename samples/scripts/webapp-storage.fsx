#r "../../src/Farmer/bin/Debug/netstandard2.0/Farmer.dll"

open Farmer
open Farmer.Builders

let myStorage = storageAccount {
    name "mystorage"
    sku Storage.Sku.Standard_LRS
    add_lifecycle_rule "cleanup" [ Storage.DeleteAfter 7<Days> ] Storage.NoRuleFilters
    add_lifecycle_rule "test" [ Storage.DeleteAfter 1<Days>; Storage.DeleteAfter 2<Days>; Storage.ArchiveAfter 1<Days>; ] [ "foo/bar" ]
}

let myWebApp = webApp {
    name "rsp-test-app2"
    sku WebApp.Sku.S1
    app_insights_off
    vnet_integration (Unmanaged { ResourceId.Type = Arm.Network.subnets; ResourceGroup = Some "rsp-test"; Subscription = None; Name = ResourceName "rsp-test-vnet"; Segments = [ResourceName "Default"] })
}

let deployment = arm {
    location Location.NorthEurope
    //add_resource myStorage
    add_resource myWebApp
}

deployment
|> Deploy.execute "rsp-test2" Deploy.NoParameters
