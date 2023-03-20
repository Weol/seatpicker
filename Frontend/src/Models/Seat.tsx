import User from "./User";

export default interface Seat {
  id: string;
  user: User | null;
  title: string;
  width: number;
  height: number;
  x: number;
  y: number;
}
