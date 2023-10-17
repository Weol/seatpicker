import Bounds from "./Bounds";
import User from "./User";

export default interface Seat {
  id: string;
  title: string;
  bounds: Bounds;
  reservedBy: User | null;
};
