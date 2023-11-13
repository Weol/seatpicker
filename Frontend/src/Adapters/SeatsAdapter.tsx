import { useAppState } from "../Contexts/AppStateContext"
import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestHook"
import { User, useAuthenticationAdapter } from "./AuthenticationAdapter"

export interface Bounds {
  x: number
  y: number
  width: number
  height: number
}

export interface Seat {
  id: string
  title: string
  bounds: Bounds
  reservedBy: User | null
}

export default function useSeats() {
  const { apiRequest, apiRequestJson } = useApiRequests()
  const { activeLan } = useAppState()
  const { loggedInUser } = useAuthenticationAdapter()
  const [seats, setSeats] = useState<Seat[] | null>(null)
  const [reservedSeat, setReservedSeat] = useState<Seat | null>(null)

  useEffect(() => {
    void reloadSeats()
  }, [activeLan])

  const reloadSeats = async () => {
    const seats = await apiRequestJson<Seat[]>(
      "GET",
      `lan/${activeLan}/seat`,
      null
    )

    setSeats(seats)

    if (loggedInUser != null) {
      let hasReservedSeat = false
      seats.forEach((seat) => {
        if (seat.reservedBy != null && seat.reservedBy.id == loggedInUser.id) {
          setReservedSeat(seat)
          hasReservedSeat = true
        }
      })

      if (!hasReservedSeat) setReservedSeat(null)
    }
  }

  const createNewSeat = (seat: Seat) => {
    return apiRequest("POST", `lan/${activeLan}/seat`, {
      title: seat.title,
      bounds: seat.bounds,
    })
  }

  return { seats, reservedSeat, reloadSeats, createNewSeat }
}
