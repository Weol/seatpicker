import ApiRequestJson from "./ApiRequest";
import Seat from "../Models/Seat";

export default function CreateReservation(seatId: string): Promise<Seat> {
  return ApiRequestJson<Seat>("POST", "/seat/reserve/" + seatId);
}
