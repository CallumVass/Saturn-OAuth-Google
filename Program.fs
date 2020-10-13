open Giraffe
open Saturn
open System.Security.Claims

let loggedIn = pipeline {
  requires_authentication (Giraffe.Auth.challenge "Google")
}

let top = router {
  pipe_through loggedIn
  get "/" (fun next ctx ->
      (ctx.User.Identity.Name |> json) next ctx)
}

// Find these information in the App Registration part of you Azure Active Directory (http://aka.ms/AppRegistrations).
let clientId = "<ClientId>"
let clientSecret = "<ClientSecret>"
let callbackPath = "/signin-google"

let jsonMappings =
    [ "id", ClaimTypes.NameIdentifier
      "displayName", ClaimTypes.Name ]

let app = application {
  use_router top
  url "http://[::]:8085/"
  memory_cache
  use_gzip
  use_google_oauth clientId clientSecret callbackPath jsonMappings
}

[<EntryPoint>]
let main _ =
  run app
  0 // return an integer exit code