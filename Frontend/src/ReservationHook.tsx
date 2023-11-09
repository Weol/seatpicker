import {useAppState} from "./AppStateContext";
import Seat from "./Models/Seat";
import useApiRequests from "./ApiRequestHook";

export default function useReservation() {
  const {apiRequest, apiRequestJson} = useApiRequests()
  const { appState } = useAppState()

  const makeReservation = (seat: Seat): Promise<Response> => {
    return apiRequest("POST", `lan/${appState.activeLan}/seat/${seat.id}/reservation`)
  }

  const deleteReservation = (seat: Seat): Promise<Response> => {
    return apiRequest("DELETE", `lan/${appState.activeLan}/seat/${seat.id}/reservation`)
  }

  const moveReservation = (fromSeat: Seat, toSeat: Seat): Promise<Response> => {
    return apiRequest("PUT", `lan/${appState.activeLan}/seat/${fromSeat.id}/reservation`, { moveToSeatId: toSeat.id })
  }

  return { makeReservation, deleteReservation, moveReservation }
}