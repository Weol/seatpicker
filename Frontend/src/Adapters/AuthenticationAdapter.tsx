import { useAppState } from "../Contexts/AppStateContext"
import Config from "../config"

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
  const { authenticationToken, setAuthenticationToken } = useAppState()
  const loggedInUser = authenticationToken && new User(authenticationToken)

  const login = async (discordToken: string): Promise<User> => {
    const authenticationToken = await makeRequest<AuthenticationToken>(
      "POST",
      `discord/authentication/login`,
      { token: discordToken }
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
      `discord/authentication/renew`,
      { refreshToken: refreshToken }
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
        const renewedAuthenticationToken = await renew(
          authenticationToken.refreshToken
        )

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

    return await fetch(Config.ApiBaseUrl + path, requestInit).then<T>(
      (response) => {
        console.log(response)
        return response.json() as T
      }
    )
  }

  return { login, logout, getToken, loggedInUser }
}
