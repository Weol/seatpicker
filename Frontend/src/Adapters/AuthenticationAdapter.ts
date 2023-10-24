import {CookiesAdapter} from "./CookiesAdapter";
import AuthenticationToken from "./Models/AuthenticationToken";
import Cookies from "universal-cookie";
import User from "../Models/User";
import Config from "../config";

const cookies = new Cookies();

export class AuthenticationAdapter {
  public static login(discordToken: string): Promise<User> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return new Promise<User>((resolve, reject) => {
      AuthenticationAdapter.MakeAnonymousRequest<AuthenticationToken>("POST", `authentication/discord/login`, { token: discordToken })
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

  private static renew(refreshToken: string): Promise<AuthenticationToken> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return new Promise<AuthenticationToken>((resolve, reject) => {
      AuthenticationAdapter.MakeAnonymousRequest<AuthenticationToken>("POST", `authentication/discord/renew`, { refreshToken: refreshToken })
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

  public static getUser(): User | null {
    let authenticationToken = this.getAuthenticationToken();
    if (authenticationToken == null) return null;

    return {
      id: authenticationToken.userId,
      nick: authenticationToken.nick,
      avatar: authenticationToken.avatar,
      roles: authenticationToken.roles
    }
  }

  public static getToken(): Promise<string | null> {
    let authenticationToken = this.getAuthenticationToken();

    return new Promise((resolve, reject) => {
      if (authenticationToken == null) {
        resolve(null)
      } else {
        if (new Date().getUTCSeconds() > new Date(authenticationToken.expiresAt).getUTCSeconds()) {
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

  private static getAuthenticationToken(): AuthenticationToken | null {
    return cookies.get("token") as AuthenticationToken ?? null;
  }

  private static async MakeAnonymousRequest<T>(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body: any): Promise<T> {
    const requestInit: RequestInit = {
      method: method,
      redirect: 'follow'
    };

    const headers = new Headers();
    if (method == "POST" || method == "PUT") {
      headers.append("Content-Type", "text/json");
      if (body != null) requestInit.body = JSON.stringify(body);
    }

    return await fetch(Config.ApiBaseUrl + path, requestInit).then<T>(response => {
      console.log(response)
      return response.json() as T
    })
  }
}
