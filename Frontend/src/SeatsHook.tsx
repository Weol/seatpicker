import {useAppState} from "./AppStateContext";
import {useEffect} from "react";

function useSeats() {
    const { seats } = useAppState();

    useEffect(() => {

    }, []);

    return seats;
}