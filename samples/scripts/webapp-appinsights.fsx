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
        SslState = SslState.SslDisabled
    }

    let cert : Arm.Web.Certificate =  {
        Location = Location.UKSouth
        SiteId = myWebApp.ResourceId
        ServicePlanId = myWebApp.ServicePlanId
        DomainName = hostNameBinding.DomainName
    }

    //let linkedTemplate : Arm.Web.LinkedDeploy = {
    //    Name = ResourceName "linkedTemplate"
    //    Location = Location.UKSouth
    //    Tags = Map<string,string>["testdeplyoyment", "abdulcodattest"]
    //    WebAppName = myWebApp.Name.Value
    //    DomainName = hostNameBinding.DomainName
    //    Certificate = cert
    //    DeploymentMode =  DeploymentMode.Incremental
    //    DeployingAlongsideResourceGroup = true
    //}

    let nested = resourceGroup{
        name "my-resource-group-name"
        //depends_on [ Arm.Web.certificates.resourceId cert.ResourceName]
        //depends_on [ Arm.Web.sites.resourceId myWebApp.Name]
        add_resource {
            hostNameBinding with SslState = SslState.Sni (ArmExpression.reference(Arm.Web.certificates, Arm.Web.certificates.resourceId cert.ResourceName).Map(sprintf "%s.Thumbprint"))
        }
    }

    //let hostNameBinding = { hostNameBinding with 
    //                        SslState = SslState.Sni (ArmExpression.reference(Arm.Web.certificates, Arm.Web.certificates.resourceId cert.ResourceName).Map(sprintf "%s.Thumbprint")) }
    
    

    arm {
        location Location.UKSouth
        add_resource myWebApp
        add_resource hostNameBinding
        add_resource cert
        add_resource nested
    }

template
//|> Writer.quickWrite "army"
|> Deploy.execute "my-resource-group-name" Deploy.NoParameters
