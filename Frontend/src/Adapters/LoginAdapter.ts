import User from '../Models/User'
import Cookies from 'universal-cookie';
import {DiscordAdapter, LoginRequest} from "./Generated";

const cookies = new Cookies();

export class LoginAdapter {
  public static getLoggedInUser() : User | null {
    return cookies.get("user")
  }

  public static logout() {
    cookies.remove("user")
    cookies.remove("token")
  }

  public static login(token: string, onSuccess: (user: User) => void) {
    var headers = new Headers();
    headers.append("Content-Type", "text/json");

    DiscordAdapter.seatpickerInfrastructureAuthenticationDiscordDiscordAuthenticationControllerPutRolesInfrastructure({token: token}).then(response => {
        let user = {
          id: response.userId,
          nick: response.nick,
          avatar: response.avatar,
          roles: response.roles
        }

        cookies.set("user", user, {path: "/"})
        cookies.set("token", response.token, {path: "/"})

        onSuccess(user);
      }
    )
  }
}
