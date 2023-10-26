import {useAppState} from "./AppStateContext";
import {Role} from "./Models/Role";
import User from "./Models/User";
import Config from "./config";

interface AuthenticationTokenModel {
  token: string;
  expiresAt: string,
  refreshToken: string;
  userId: string;
  nick: string;
  avatar: string | null;
  roles: Role[];
}

export default function useAuthentication() {
  const { appState, setAppState } = useAppState()

  const login = (discordToken: string): Promise<User> => {
    return new Promise<User>((resolve, reject) => {
      makeRequest<AuthenticationTokenModel>("POST", `authentication/discord/login`, { token: discordToken })
      .then(authenticationToken => {
        if (authenticationToken == null) {
          reject()
        } else {
          const user = {
            id: authenticationToken.userId,
            nick: authenticationToken.nick,
            avatar: authenticationToken.avatar,
            roles: authenticationToken.roles
          };

          setAppState({ ...appState, loggedInUser: user, authenticationToken: authenticationToken })
        }

      })
      .catch(reason => reject(reason));
    })
  }

  const renew = (refreshToken: string): Promise<AuthenticationTokenModel> => {
    return new Promise<AuthenticationTokenModel>((resolve, reject) => {
      makeRequest<AuthenticationTokenModel>("POST", `authentication/discord/renew`, { refreshToken: refreshToken })
      .then(authenticationToken => {
        if (authenticationToken == null) {
          reject()
        } else {
          const user = {
            id: authenticationToken.userId,
            nick: authenticationToken.nick,
            avatar: authenticationToken.avatar,
            roles: authenticationToken.roles
          };

          setAppState({ ...appState, loggedInUser: user, authenticationToken: authenticationToken })
        }
      })
      .catch(reason => reject(reason));
    })
  }

  const token = (): Promise<string | null> => {
    return new Promise((resolve, reject) => {
      if (appState.authenticationToken == null) {
        resolve(null)
      } else {
        if (new Date() > new Date(appState.authenticationToken.expiresAt)) {
          renew(appState.authenticationToken.refreshToken)
          .then(renewedAuthenticationToken => {
            resolve(renewedAuthenticationToken.token)
          })
          .catch(reason => reject(reason))
        }

        resolve(appState.authenticationToken.token)
      }
    })
  }

  async function makeRequest<T>(
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

  return { login, token }
}
