import Cookies from 'universal-cookie';

const cookies = new Cookies();

export default function GetLoggedInUser() {
  return cookies.get("user")
}