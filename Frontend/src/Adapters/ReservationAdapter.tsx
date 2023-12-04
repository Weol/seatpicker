import useApiRequests from "./ApiRequestHook"
import { User } from "./AuthenticationAdapter"
import { Lan } from "./LanAdapter"
import { Seat } from "./SeatsAdapter"
import useVersionId from "./VersionIdHook"

export default function useReservationAdapter(lan: Lan | null) {
  const { apiRequest } = useApiRequests()
  const { invalidate } = useVersionId("Seats")

  const makeReservation = async (seat: Seat) => {
    await apiRequest("POST", `lan/${lan?.id}/seat/${seat.id}/reservation`)
    invalidate()
  }

  const deleteReservation = async (seat: Seat) => {
    await apiRequest("DELETE", `lan/${lan?.id}/seat/${seat.id}/reservation`)
    invalidate()
  }

  const moveReservation = async (fromSeat: Seat, toSeat: Seat) => {
    await apiRequest("PUT", `lan/${lan?.id}/seat/${fromSeat.id}/reservation`, {
      toSeatId: toSeat.id,
    })
    invalidate()
  }

  const makeReservationFor = async (seat: Seat, user: User) => {
    await apiRequest("POST", `lan/${lan?.id}/seat/${seat.id}/reservationmanagement`, {
      userId: user.id,
    })
    invalidate()
  }

  const moveReservationFor = async (fromSeat: Seat, toSeat: Seat) => {
    await apiRequest("PUT", `lan/${lan?.id}/seat/${fromSeat.id}/reservationmanagement`, {
      toSeatId: toSeat.id,
      userId: fromSeat.reservedBy?.id,
    })
    invalidate()
  }

  const deleteReservationFor = async (seat: Seat) => {
    await apiRequest("DELETE", `lan/${lan?.id}/seat/${seat.id}/reservationmanagement`)
    invalidate()
  }

  return {
    makeReservation,
    makeReservationFor,
    deleteReservation,
    deleteReservationFor,
    moveReservation,
    moveReservationFor,
  }
}
