#r "../../src/Farmer/bin/Debug/netstandard2.0/Farmer.dll"

open Farmer
open Farmer.Builders
open Farmer.Arm
open Farmer.Arm.ResourceGroup

let template =
    let myWebApp = webApp {
        name "codat-devopstest"
        sku WebApp.Sku.F1
        app_insights_off
        runtime_stack Runtime.DotNetCore31
    }

    let hostNameBinding : Arm.Web.HostNameBinding = {
        Location = Location.UKSouth
        SiteId = myWebApp.ResourceId
        DomainName = "devops-test.codat.io"
    }

    let cert : Arm.Web.Certificate =  {
        Location = Location.UKSouth
        SiteId = myWebApp.ResourceId
        ServicePlanId = myWebApp.ServicePlanId
        DomainName = hostNameBinding.DomainName
    }

    let linkedTemplate : Arm.Web.LinkedDeploy = {
        Name = ResourceName "linkedTemplate"
        Location = Location.UKSouth
        Tags = Map<string,string>["testdeplyoyment", "abdulcodattest"]
        WebAppName = myWebApp.Name
        DomainName = ResourceName hostNameBinding.DomainName
        Certificate = cert
        DeploymentMode =  DeploymentMode.Incremental
        DeployingAlongsideResourceGroup = true
    }

    //let hostNameBinding = { hostNameBinding with 
    //                        SslState = SslState.Sni (ArmExpression.reference(Arm.Web.certificates, Arm.Web.certificates.resourceId cert.ResourceName).Map(sprintf "%s.Thumbprint")) }
    
    

    arm {
        location Location.UKSouth
        add_resource myWebApp
        add_resource hostNameBinding
        add_resource cert
        add_resource linkedTemplate
    }

template
//|> Writer.quickWrite "army"
|> Deploy.execute "my-resource-group-name" Deploy.NoParameters
