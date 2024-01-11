import { atom, useRecoilState } from "recoil"
import Config from "../config"
import { RedirectUrl } from "./RedirectToDiscordLogin"

export type AuthenticationToken = {
  token: string
  expiresAt: string
  refreshToken: string
  guildId: string
  userId: string
  nick: string
  avatar: string | null
  roles: Role[]
}

export class AuthAdapter {
  private static GetTokenFromLocalStorage(): AuthenticationToken | null {
    const token = localStorage.getItem("authenticationToken")
    if (token) {
      return JSON.parse(token) as AuthenticationToken
    }
    return null
  }

  private static SaveTokenToLocalStorage(token: AuthenticationToken | null) {
    if (token) {
      return localStorage.setItem("authenticationToken", JSON.stringify(token))
    }
    localStorage.removeItem("authenticationToken")
  }

  static GetLoggedInUserFromLocalStorage() {
    const token = AuthAdapter.GetTokenFromLocalStorage()

    if (token == null) return null
    return new User(token)
  }

  private static async Renew(token: AuthenticationToken) {
    const response = await AuthAdapter.MakeRequest("POST", `authentication/discord/renew`, {
      refreshToken: token.refreshToken,
      guildId: token.guildId,
    })

    if (response.status == 200) {
      const authenticationToken = (await response.json()) as AuthenticationToken
      if (authenticationToken == null) {
        throw "authenticationToken is null"
      } else {
        AuthAdapter.SaveTokenToLocalStorage(authenticationToken)
        return authenticationToken
      }
    } else {
      AuthAdapter.Logout()
      throw response
    }
  }

  static async Login(discordToken: string, guildId: string) {
    const response = await AuthAdapter.MakeRequest("POST", `authentication/discord/login`, {
      token: discordToken,
      guildId: guildId,
      redirectUrl: RedirectUrl,
    })

    if (response.status == 200) {
      const authenticationToken = (await response.json()) as AuthenticationToken
      if (authenticationToken == null) {
        throw "Authentication token is null"
      } else {
        AuthAdapter.SaveTokenToLocalStorage(authenticationToken)
        return new User(authenticationToken)
      }
    } else {
      throw response
    }
  }

  static Logout() {
    AuthAdapter.SaveTokenToLocalStorage(null)
  }

  static async GetToken() {
    const authenticationToken = AuthAdapter.GetTokenFromLocalStorage()
    if (authenticationToken == null) {
      return null
    } else {
      const unixNow = Math.floor(Date.now() / 1000)
      if (unixNow > Number(authenticationToken.expiresAt) - 10) {
        const renewedAuthenticationToken = await AuthAdapter.Renew(authenticationToken)

        return renewedAuthenticationToken.token
      }

      return authenticationToken.token
    }
  }

  static MakeRequest(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body: unknown
  ): Promise<Response> {
    const tenantStorage = localStorage.getItem("activeGuildId")
    if (tenantStorage == null) throw "tenant is null"
    const tenant = JSON.parse(tenantStorage) as string

    const requestInit: RequestInit = {
      method: method,
      redirect: "follow",
    }

    const headers = new Headers()
    if (method == "POST" || method == "PUT") {
      if (typeof body !== "undefined") requestInit.body = JSON.stringify(body)
      headers.append("Content-Type", "text/json")
    }
    headers.append("Tenant-Id", tenant)
    requestInit.headers = headers

    return fetch(Config.ApiBaseUrl + "/" + path, requestInit)
  }
}

export enum Role {
  ADMIN = "Admin",
  OPERATOR = "Operator",
  USER = "User",
}

export class User {
  id: string
  name: string
  avatar: string | null
  roles: Role[]

  constructor(authenticationToken: AuthenticationToken) {
    this.id = authenticationToken.userId
    this.name = authenticationToken.nick
    this.avatar = authenticationToken.avatar
    this.roles = authenticationToken.roles
  }

  isInRole(role: Role): boolean {
    return this.roles.includes(role)
  }
}

export const loggedInUserAtom = atom<User | null>({
  key: "loggedInUser",
  default: AuthAdapter.GetLoggedInUserFromLocalStorage(),
})

export function useAuth() {
  const [loggedInUser, setLoggedInUser] = useRecoilState(loggedInUserAtom)

  async function login(discordToken: string, guildId: string) {
    const user = await AuthAdapter.Login(discordToken, guildId)
    setLoggedInUser(user)
  }

  function logout() {
    AuthAdapter.Logout()
    setLoggedInUser(null)
  }

  return { loggedInUser, logout, login }
}
