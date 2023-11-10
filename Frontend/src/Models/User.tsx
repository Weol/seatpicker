import {Role} from "./Role";

export default interface User {
  id: string;
  name: string;
  avatar: string | null;
  roles: Role[]
}