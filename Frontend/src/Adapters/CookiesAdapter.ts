import Cookies from 'universal-cookie';

const cookies = new Cookies();

export class CookiesAdapter {
  public static setCurrentLan(lanId : string) {
    cookies.set("currentLan", lanId)
  }

  public static unsetCurrentLan() {
    cookies.remove("currentLan")
  }

  public static getCurrentLan() : string | null  {
    return cookies.get("currentLan")
  }
}
