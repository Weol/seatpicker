import { useLocalStorage } from "usehooks-ts"
import Config from "../config"
import { useActiveGuildId } from "./ActiveGuildAdapter"
import { RedirectUrl } from "./RedirectToDiscordLogin"

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

export interface AuthenticationToken {
  token: string
  expiresAt: string
  refreshToken: string
  userId: string
  nick: string
  avatar: string | null
  roles: Role[]
}

export function useAuthenticationAdapter() {
  const { activeGuildId } = useActiveGuildId()
  const [authenticationToken, setAuthenticationToken] = useLocalStorage<AuthenticationToken | null>(
    "authenticationToken",
    null
  )
  const loggedInUser = authenticationToken == null ? null : new User(authenticationToken)

  const login = async (discordToken: string): Promise<User> => {
    const authenticationToken = await makeRequest<AuthenticationToken>(
      "POST",
      `authentication/discord/login`,
      { token: discordToken, guildId: activeGuildId, redirectUrl: RedirectUrl }
    )

    if (authenticationToken == null) {
      throw "Authentication token is null"
    } else {
      setAuthenticationToken(authenticationToken)
      return new User(authenticationToken)
    }
  }

  const renew = async (refreshToken: string): Promise<AuthenticationToken> => {
    const authenticationToken = await makeRequest<AuthenticationToken>(
      "POST",
      `authentication/discord/renew`,
      { refreshToken: refreshToken, guildId: activeGuildId }
    )
    if (authenticationToken == null) {
      throw "authenticationToken is null"
    } else {
      setAuthenticationToken(authenticationToken)
      return authenticationToken
    }
  }

  const logout = () => {
    setAuthenticationToken(null)
  }

  const getToken = async (): Promise<string | null> => {
    if (authenticationToken == null) {
      return null
    } else {
      if (new Date() > new Date(authenticationToken.expiresAt)) {
        const renewedAuthenticationToken = await renew(authenticationToken.refreshToken)

        return renewedAuthenticationToken.token
      }

      return authenticationToken.token
    }
  }

  async function makeRequest<T>(
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

    return await fetch(Config.ApiBaseUrl + "/" + path, requestInit).then<T>((response) => {
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

  return { login, logout, getToken, loggedInUser }
}
