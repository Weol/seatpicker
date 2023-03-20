import User from "../Models/User";
import Config from "../config"
import Cookies from "universal-cookie";

const cookies = new Cookies();

type TokenContract = {
  Token: string
}

function ParseJwt(token: string) {
  var base64Url = token.split('.')[1];
  var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
  }).join(''));

  return JSON.parse(jsonPayload);
}

export default function LoginWithDiscordToken(token: string, onSuccess: (user: User) => void) {
  var headers = new Headers();
  headers.append("Content-Type", "text/json");

  let contract: TokenContract = {Token: token}

  fetch(Config.ApiBaseUrl + "/token", {
    method: 'POST',
    headers: headers,
    body: JSON.stringify(contract),
    redirect: 'follow'
  })
    .then(response => response.json())
    .then(result => {
      console.log(result)
      let token = result["token"]

      let parsedJwt = ParseJwt(token)
      let user = {
        id: parsedJwt.spu_id,
        nick: parsedJwt.spu_nick,
        avatar: parsedJwt.spu_avatar,
        roles: parsedJwt.role
      }

      cookies.set("user", user, {path: "/"})
      cookies.set("token", token, {path: "/"})

      onSuccess(user)
    })
    .catch(result => {
      console.log(result)
    })
}