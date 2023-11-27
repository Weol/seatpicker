import { useState } from "react"
import { useAlerts } from "../Contexts/AlertContext"
import useReservationAdapter from "../Adapters/ReservationAdapter"
import { useDialogs } from "../Contexts/DialogContext"
import { useAuthenticationAdapter } from "../Adapters/AuthenticationAdapter"
import { Seat, useSeats } from "../Adapters/SeatsAdapter"
import { Lan } from "../Adapters/LanAdapter"

export function SeatsWithLan(props: { activeLan: Lan }) {
  const { alertWarning, alertInfo, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { loggedInUser } = useAuthenticationAdapter()
  const { seats, reservedSeat } = useSeats(props.activeLan)
  const { makeReservation, deleteReservation, moveReservation } =
    useReservationAdapter(props.activeLan)
  const [freeze, setFreeze] = useState<boolean>(false)

  const onReserve = (seat: Seat) => {
    throw new Error("Function not implemented.")
  }
}
