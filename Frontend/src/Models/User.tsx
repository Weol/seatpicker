import {Role} from "./Role";

export default interface User {
  id: string;
  nick: string;
  avatar: string | null;
  roles: Role[]
}