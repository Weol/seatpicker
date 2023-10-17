import ApiRequestJson, {ApiRequest} from "./ApiRequest";
import {CookiesAdapter} from "./CookiesAdapter";
import AuthenticationToken from "./Models/AuthenticationToken";
import User from "./Models/User";
import Cookies from "universal-cookie";

const cookies = new Cookies();

export class AuthenticationAdapter {
  public static login(discordToken : string) : Promise<User> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return new Promise<User>((resolve, reject) => {
      ApiRequestJson<AuthenticationToken>("POST", `authentication/discord/login`, { token: discordToken })
        .then(authenticationToken => {
          cookies.set("token", authenticationToken);

          let user = this.getUser()
          if (user == null) {
            reject("token cookie is null")
          } else {
            resolve(user)
          }
        })
        .catch(reason => reject(reason));
    })
  }

  private static renew(refreshToken : string) : Promise<AuthenticationToken> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return new Promise<AuthenticationToken>((resolve, reject) => {
      ApiRequestJson<AuthenticationToken>("POST", `authentication/discord/renew`, { refreshToken: refreshToken })
        .then(authenticationToken => {
          cookies.set("token", authenticationToken);

          if (authenticationToken == null) {
            reject("token cookie is null")
          } else {
            resolve(authenticationToken)
          }
        })
        .catch(reason => reject(reason));
    })
  }

  public static getUser() : User | null {
    let authenticationToken = this.getAuthenticationToken();
    if (authenticationToken == null) return null;

    return {
      id: authenticationToken.userId,
      nick: authenticationToken.nick,
      avatar: authenticationToken.avatar,
      roles: authenticationToken.roles
    }
  }

  public static getToken() : Promise<string> {
    let authenticationToken = this.getAuthenticationToken();

    return new Promise((resolve, reject) => {
      if (authenticationToken == null) {
        reject("token cookie is null");
      } else {
        if (new Date().getUTCSeconds() > authenticationToken.expiresAt.getUTCSeconds()) {
          this.renew(authenticationToken.refreshToken)
            .then(renewedAuthenticationToken => {
              resolve(renewedAuthenticationToken.token)
            })
            .catch(reason => reject(reason))
        }

        resolve(authenticationToken.token)
      }
    })
  }

  private static getAuthenticationToken() : AuthenticationToken | null {
    return cookies.get("token") as AuthenticationToken ?? null;
  }
}
