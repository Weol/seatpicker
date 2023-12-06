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
  expiresAt: number
  refreshToken: string
  userId: string
  nick: string
  avatar: string | null
  roles: Role[]
}

function getUserFromAuthenticationToken(token: AuthenticationToken | null) {
  if (token == null) return null
  if (Object.keys(token).length == 0) return null
  return new User(token)
}

export function useAuthenticationAdapter() {
  const { activeGuildId } = useActiveGuildId()
  const [authenticationToken, setAuthenticationToken] = useLocalStorage<AuthenticationToken | null>(
    "authenticationToken",
    null
  )
  const loggedInUser = getUserFromAuthenticationToken(authenticationToken)

  const login = async (discordToken: string): Promise<User> => {
    const response = await makeRequest("POST", `authentication/discord/login`, {
      token: discordToken,
      guildId: activeGuildId,
      redirectUrl: RedirectUrl,
    })

    if (response.status == 200) {
      const authenticationToken = (await response.json()) as AuthenticationToken
      if (authenticationToken == null) {
        throw "Authentication token is null"
      } else {
        setAuthenticationToken(authenticationToken)
        return new User(authenticationToken)
      }
    } else {
      throw response
    }
  }

  const renew = async (refreshToken: string): Promise<AuthenticationToken> => {
    const response = await makeRequest("POST", `authentication/discord/renew`, {
      refreshToken: refreshToken,
      guildId: activeGuildId,
    })

    if (response.status == 200) {
      const authenticationToken = (await response.json()) as AuthenticationToken
      if (authenticationToken == null) {
        throw "authenticationToken is null"
      } else {
        setAuthenticationToken(authenticationToken)
        return authenticationToken
      }
    } else {
      logout()
      throw response
    }
  }

  const logout = () => {
    setAuthenticationToken(null)
  }

  const getToken = async (): Promise<string | null> => {
    if (authenticationToken == null) {
      return null
    } else {
      const unixNow = Math.floor(Date.now() / 1000)
      if (unixNow > authenticationToken.expiresAt - 10) {
        const renewedAuthenticationToken = await renew(authenticationToken.refreshToken)

        return renewedAuthenticationToken.token
      }

      return authenticationToken.token
    }
  }

  async function makeRequest(
    method: "POST" | "DELETE" | "GET" | "PUT",
    path: string,
    body: unknown
  ): Promise<Response> {
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

    return fetch(Config.ApiBaseUrl + "/" + path, requestInit)
  }

  return { login, logout, getToken, loggedInUser }
}
