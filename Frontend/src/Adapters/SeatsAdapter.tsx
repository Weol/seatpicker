import { useEffect, useState } from "react"
import useApiRequests from "./ApiRequestAdapter"
import { Lan } from "./LanAdapter"
import { User, useAuthenticationAdapter } from "./LoggedInUserAdapter"
import useVersionId from "./VersionIdHook"

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

export function useSeats(lan: Lan | null) {
  const { apiRequest } = useApiRequests()
  const { loggedInUser } = useAuthenticationAdapter()
  const { versionId } = useVersionId("Seats")
  const [seats, setSeats] = useState<Seat[] | null>(null)
  const reservedSeat = findSeatReservedBy(seats, loggedInUser)

  function findSeatReservedBy(seats: Seat[] | null, loggedInUser: User | null) {
    if (seats && loggedInUser) {
      for (let i = 0; i < seats.length; i++) {
        const seat = seats[i]
        if (seat.reservedBy != null && seat.reservedBy.id == loggedInUser.id) {
          return seat
        }
      }
    }
    return null
  }

  useEffect(() => {
    loadSeats()
  }, [lan?.id, versionId])

  async function loadSeats() {
    const response = await apiRequest("GET", `lan/${lan?.id}/seat`)

    const seats = (await response.json()) as Seat[]
    setSeats(seats)
  }

  return { seats, reservedSeat }
}

export function useSeatAdapter(lan: Lan) {
  const { apiRequest } = useApiRequests()

  const createNewSeat = (seat: Seat) => {
    return apiRequest("POST", `lan/${lan.id}/seat`, {
      title: seat.title,
      bounds: seat.bounds,
    })
  }

  return { createNewSeat }
}
