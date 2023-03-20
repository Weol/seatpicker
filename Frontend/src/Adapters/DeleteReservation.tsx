import {ApiRequest} from "./ApiRequest";

export default function DeleteReservation(seatId: string): Promise<Response> {
  return ApiRequest("POST", "/seat/unreserve/" + seatId);
}
