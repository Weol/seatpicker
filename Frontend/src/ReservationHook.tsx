import {useAppState} from "./AppStateContext";
import {ApiRequest} from "./Adapters/ApiRequest";
import Seat from "./Models/Seat";

export default function useReservation() {
  const { appState } = useAppState();

  const makeReservation = (seat: Seat): Promise<Response> => {
    return ApiRequest("POST", `lan/${appState.activeLan}/seat/${seat.id}/reservation`, null)
  }

  const deleteReservation = (seat: Seat): Promise<Response> => {
    return ApiRequest("DELETE", `lan/${appState.activeLan}/seat/${seat.id}/reservation`, null)
  }

  const moveReservation = (fromSeat: Seat, toSeat: Seat): Promise<Response> => {
    return ApiRequest("PUT", `lan/${appState.activeLan}/seat/${fromSeat.id}/reservation`, null,{ moveToSeatId: toSeat.id })
  }

  return { makeReservation, deleteReservation, moveReservation }
}