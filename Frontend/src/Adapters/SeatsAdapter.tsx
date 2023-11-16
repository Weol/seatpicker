import { useAppState } from "../Contexts/AppStateContext"
import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestHook"
import { User, useAuthenticationAdapter } from "./AuthenticationAdapter"
import { useApi } from "../Contexts/ApiContext"

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
  const { apiRequest } = useApiRequests()
  const { activeLan } = useAppState()
  const { loggedInUser } = useAuthenticationAdapter()
  const { seats, setSeats } = useApi()
  const [reservedSeat, setReservedSeat] = useState<Seat | null>(null)

  useEffect(() => {
    if (seats == null) {
      reloadSeats()
    }
  }, [])

  const reloadSeats = async () => {
    const response = await apiRequest("GET", `lan/${activeLan}/seat`)

    const seats = (await response.json()) as Seat[]
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

  return { seats, reservedSeat, createNewSeat }
}
