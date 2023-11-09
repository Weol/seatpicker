import {useAppState} from "./AppStateContext";
import {useEffect, useState} from "react";
import Seat from "./Models/Seat";
import User from "./Models/User";
import useApiRequests from "./ApiRequestHook";

export default function useSeats() {
  const {apiRequest, apiRequestJson} = useApiRequests()
  const {appState} = useAppState();
  const [seats, setSeats] = useState<Seat[]>([]);
  const [reservedSeat, setReservedSeat] = useState<Seat | null>(null);

  useEffect(() => {
    reloadSeats()
  }, [appState.activeLan]);

  const reloadSeats = async () => {
    let seats = await apiRequestJson<Seat[]>("GET", `lan/${appState.activeLan}/seat`, null)
    
    setSeats(seats)
    
    if (appState.loggedInUser != null) {
      let loggedInUser = appState.loggedInUser

      let hasReservedSeat = false;
      seats.forEach(seat => {
        if (seat.reservedBy != null && seat.reservedBy.id == loggedInUser.id) {
          setReservedSeat(seat)
          hasReservedSeat = true
        }
      })

      if (!hasReservedSeat) setReservedSeat(null);
    }
  }

  const createNewSeat = async (seat: Seat) => {
    await apiRequest("POST", `lan/${appState.activeLan}/seat`, {title: seat.title, bounds: seat.bounds})
  }

  const setSeatReservedBy = (seat: Seat, user: User | null) => {
    let newSeats = [...seats]
    newSeats.forEach(a => {
      if (a.id == seat.id) seat.reservedBy = user
    })
    setSeats(newSeats)
  }

  return {seats, reservedSeat, reloadSeats, createNewSeat, setSeatReservedBy}
}