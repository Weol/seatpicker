import useApiRequests from "./ApiRequestHook"
import { Lan } from "./LanAdapter"
import { Seat } from "./SeatsAdapter"

export default function useReservationAdapter(lan: Lan | null) {
  const { apiRequest } = useApiRequests()

  const makeReservation = (seat: Seat) => {
    return apiRequest("POST", `lan/${lan?.id}/seat/${seat.id}/reservation`)
  }

  const deleteReservation = (seat: Seat) => {
    return apiRequest("DELETE", `lan/${lan?.id}/seat/${seat.id}/reservation`)
  }

  const moveReservation = (fromSeat: Seat, toSeat: Seat) => {
    return apiRequest("PUT", `lan/${lan?.id}/seat/${fromSeat.id}/reservation`, {
      moveToSeatId: toSeat.id,
    })
  }

  return { makeReservation, deleteReservation, moveReservation }
}
