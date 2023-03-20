import {ApiRequest} from "./ApiRequest";

export default function DeleteReservation(currentSeatId: string, newSeatId: string): Promise<Response> {
  return ApiRequest("POST", "/seat/replace/" + currentSeatId + "/" + newSeatId);
}
