import Seat from "../Models/Seat";
import ApiRequestJson from "./ApiRequest";

export default function GetAllSeats(): Promise<Seat[]> {
  return ApiRequestJson<Seat[]>("GET", "/seat");
}
