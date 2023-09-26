import User from '../Models/User'
import Cookies from 'universal-cookie';
import { DiscordAuthenticationAdapter, LoginRequest } from "./Generated";

const cookies = new Cookies();

export class LoginAdapter {
    public static getLoggedInUser() {
        return cookies.get("user")
    }

    public static logout() {
        cookies.remove("user")
        cookies.remove("token")
    }

    public static login(token: string, onSuccess: (user: User) => void) {
        var headers = new Headers();
        headers.append("Content-Type", "text/json");

        DiscordAuthenticationAdapter.postDiscordLogin({token: token})
          .then>

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

    private static parseJwt(token: string) {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));

        return JSON.parse(jsonPayload);
    }
}
