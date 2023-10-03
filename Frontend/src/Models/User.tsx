import {Role} from "../Adapters/Generated";

export default interface User {
  id: string;
  nick: string;
  avatar: string | null;
  roles: Array<Role>
}