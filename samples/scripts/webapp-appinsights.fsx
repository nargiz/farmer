#r "../../src/Farmer/bin/Debug/netstandard2.0/Farmer.dll"

open Farmer
open Farmer.Builders
open Farmer.Arm

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

    let hostNameBinding = { hostNameBinding with 
            SslState = SslState.Sni (ArmExpression.reference(Arm.Web.certificates, Arm.Web.certificates.resourceId cert.ResourceName).Map(sprintf "%s.Thumbprint")) }
    

    arm {
        location Location.UKSouth
        add_resource myWebApp
        add_resource hostNameBinding
        //add_resource cert
    }

template
//|> Writer.quickWrite "army"
|> Deploy.execute "my-resource-group-name" Deploy.NoParameters
