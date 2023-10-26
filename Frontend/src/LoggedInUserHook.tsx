import {useAppState} from "./AppStateContext";

export default function useLoggedInUser() {
    const { appState } = useAppState();

    return appState.loggedInUser
}