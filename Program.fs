open Giraffe
open Saturn
open System.Security.Claims
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open System

let loggedIn = pipeline {
  requires_authentication (Giraffe.Auth.challenge "Google")
}

let top = router {
  pipe_through loggedIn
  get "/" (fun next ctx ->
      (ctx.User.Identity.Name |> json) next ctx)
}

// Find these information in the App Registration part of you Azure Active Directory (http://aka.ms/AppRegistrations).
let clientId = ""
let clientSecret = ""
let callbackPath = "/signin-google"

let jsonMappings =
    [ "id", ClaimTypes.NameIdentifier
      "displayName", ClaimTypes.Name ]


let app = application {
  use_router top
  url "http://[::]:8085/"
  memory_cache
  use_gzip
  use_google_oauth_with_config (fun opts ->
                                        opts.ClientId <- clientId
                                        opts.ClientSecret <- clientSecret
                                        jsonMappings
                                        |> Seq.iter (fun (k, v) -> opts.ClaimActions.MapJsonKey(v, k))
                                        opts.ClaimActions.MapJsonSubKey("urn:google:image:url", "image", "url")
                                        opts.CorrelationCookie.SameSite <- SameSiteMode.Lax
                                        let ev = opts.Events
                                        ev.OnCreatingTicket <- Func<_, _> parseAndValidateOauthTicket
  )
}

[<EntryPoint>]
let main _ =
  run app
  0 // return an integer exit code