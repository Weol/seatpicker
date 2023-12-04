import { atom, useRecoilState } from "recoil"
import Config from "../config"

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
  private static GetTokenFromLocalStorage() {
    const token = localStorage.getItem("authenticationToken")
    if (token) {
      return JSON.parse(token)
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
    const authenticationToken = AuthAdapter.GetTokenFromLocalStorage()
    return authenticationToken ? new User(authenticationToken) : null
  }

  private static async Renew(token: AuthenticationToken) {
    const renewedToken = await AuthAdapter.MakeRequest<AuthenticationToken>(
      "POST",
      `authentication/discord/renew`,
      { refreshToken: token.refreshToken, guildId: token }
    )
    if (renewedToken == null) {
      throw "authenticationToken is null"
    } else {
      AuthAdapter.SaveTokenToLocalStorage(token)
      return token
    }
  }

  static async Login(discordToken: string, guildId: string) {
    const authenticationToken = await AuthAdapter.MakeRequest<AuthenticationToken>(
      "POST",
      `authentication/discord/login`,
      { token: discordToken, guildId: guildId }
    )

    if (authenticationToken == null) {
      throw "Authentication token is null"
    } else {
      AuthAdapter.SaveTokenToLocalStorage(authenticationToken)
      return new User(authenticationToken)
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
      if (new Date() > new Date(authenticationToken.expiresAt)) {
        const renewedAuthenticationToken = await AuthAdapter.Renew(authenticationToken.refreshToken)

        return renewedAuthenticationToken.token
      }

      return authenticationToken.token
    }
  }

  static async MakeRequest<T>(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body: unknown
  ): Promise<T> {
    const requestInit: RequestInit = {
      method: method,
      redirect: "follow",
    }

    const headers = new Headers()
    if (method == "POST" || method == "PUT") {
      if (typeof body !== "undefined") requestInit.body = JSON.stringify(body)
      headers.append("Content-Type", "text/json")
    }
    requestInit.headers = headers

    return await fetch(Config.ApiBaseUrl + path, requestInit).then<T>((response) => {
      const body = response.json() as T
      console.log({
        body: body,
        status: response.status,
        url: response.url,
        method: method,
        headers: response.headers,
      })
      return body
    })
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

export function useAuthAdapter() {
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
