import {useAppState} from "./AppStateContext";
import {ApiRequest} from "./Adapters/ApiRequest";
import Seat from "./Models/Seat";

export default function useReservation() {
  const { appState } = useAppState();

  const getAuthToken = (): string | null => {
    return appState.authenticationToken ? appState.authenticationToken.token : null
  }

  const makeReservation = (seat: Seat): Promise<Response> => {
    return ApiRequest("POST", `lan/${appState.activeLan}/seat/${seat.id}/reservation`, getAuthToken())
  }

  const deleteReservation = (seat: Seat): Promise<Response> => {
    return ApiRequest("DELETE", `lan/${appState.activeLan}/seat/${seat.id}/reservation`, getAuthToken())
  }

  const moveReservation = (fromSeat: Seat, toSeat: Seat): Promise<Response> => {
    return ApiRequest("PUT", `lan/${appState.activeLan}/seat/${fromSeat.id}/reservation`, getAuthToken(),{ moveToSeatId: toSeat.id })
  }

  return { makeReservation, deleteReservation, moveReservation }
}