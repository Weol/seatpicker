import {ApiRequest} from "./ApiRequest";
import {CookiesAdapter} from "./CookiesAdapter";

export class ReservationAdapter {
  public static reserve(seatId : string) : Promise<Response> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return ApiRequest("POST", `lan/${lanId}/seat/${seatId}/reservation`);
  }

  public static unreserve(seatId : string) : Promise<Response> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return ApiRequest("DELETE", `lan/${lanId}/seat/${seatId}/reservation`);
  }

  public static move(fromSeatId : string, toSeatId : string) : Promise<Response> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return ApiRequest("DELETE", `lan/${lanId}/seat/${fromSeatId}/reservation`, { moveToSeatId: toSeatId });
  }

}
