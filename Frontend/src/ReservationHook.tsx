import {useAppState} from "./AppStateContext";
import {useEffect} from "react";

function useReservation() {
    const { activeLan, loggedInUser } = useAppState();

    const makeReservation(seat : Seat) : Promise<Seat>

    return seats;
}