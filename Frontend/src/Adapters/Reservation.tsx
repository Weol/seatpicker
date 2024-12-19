import { ApiRequest } from "./ApiRequest"
import { Lan, Seat, User } from "./Models"

export default function useReservationAdapter(guildId: string, lan: Lan) {
  const makeReservation = async (seat: Seat) => {
    await ApiRequest("POST", `guild/${guildId}/lan/${lan.id}/seat/${seat.id}/reservation`)
  }

  const deleteReservation = async (seat: Seat) => {
    await ApiRequest("DELETE", `guild/${guildId}/lan/${lan.id}/seat/${seat.id}/reservation`)
  }

  const moveReservation = async (fromSeat: Seat, toSeat: Seat) => {
    await ApiRequest("PUT", `guild/${guildId}/lan/${lan.id}/seat/${fromSeat.id}/reservation`, {
      toSeatId: toSeat.id,
    })
  }

  const makeReservationFor = async (seat: Seat, user: User) => {
    await ApiRequest(
      "POST",
      `guild/${guildId}/lan/${lan.id}/seat/${seat.id}/reservationmanagement`,
      {
        userId: user.id,
      }
    )
  }

  const moveReservationFor = async (fromSeat: Seat, toSeat: Seat) => {
    await ApiRequest(
      "PUT",
      `guild/${guildId}/lan/${lan.id}/seat/${fromSeat.id}/reservationmanagement`,
      {
        toSeatId: toSeat.id,
      }
    )
  }

  const deleteReservationFor = async (seat: Seat) => {
    await ApiRequest(
      "DELETE",
      `guild/${guildId}/lan/${lan.id}/seat/${seat.id}/reservationmanagement`
    )
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
