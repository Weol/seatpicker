import ApiRequestJson from "./ApiRequest";
import {CookiesAdapter} from "./CookiesAdapter";
import Seat from "./Models/Seat";

export class ReservationAdapter {
  public static getAllSeats() : Promise<Seat[]> {
    let lanId = CookiesAdapter.getCurrentLan()
    if (lanId == null) throw "No current lan is set";

    return ApiRequestJson<Seat[]>("GET", `lan/${lanId}/seat`);
  }
}
