import Cookies from 'universal-cookie';

const cookies = new Cookies();

export default function LogOutLoggedInUser() {
  cookies.remove("user")
  cookies.remove("token")
}