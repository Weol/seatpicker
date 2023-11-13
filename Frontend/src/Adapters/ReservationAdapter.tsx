import { useAppState } from "../Contexts/AppStateContext"
import useApiRequests from "./ApiRequestHook"
import { Seat } from "./SeatsAdapter"

export default function useReservationAdapter() {
  const { apiRequest } = useApiRequests()
  const { activeLan } = useAppState()

  const makeReservation = (seat: Seat) => {
    return apiRequest("POST", `lan/${activeLan}/seat/${seat.id}/reservation`)
  }

  const deleteReservation = (seat: Seat) => {
    return apiRequest("DELETE", `lan/${activeLan}/seat/${seat.id}/reservation`)
  }

  const moveReservation = (fromSeat: Seat, toSeat: Seat) => {
    return apiRequest(
      "PUT",
      `lan/${activeLan}/seat/${fromSeat.id}/reservation`,
      { moveToSeatId: toSeat.id }
    )
  }

  return { makeReservation, deleteReservation, moveReservation }
}
