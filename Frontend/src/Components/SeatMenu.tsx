import Divider from "@mui/material/Divider"
import MenuItem from "@mui/material/MenuItem"
import ListItemText from "@mui/material/ListItemText"
import ListItemIcon from "@mui/material/ListItemIcon"
import Menu, { MenuProps } from "@mui/material/Menu"
import { DiscordUserAvatar } from "./DiscordAvatar"
import { AddCircleOutline, AddLink, Delete, Shuffle } from "@mui/icons-material"
import Typography from "@mui/material/Typography"
import {
  Role,
  User,
  useAuthenticationAdapter,
} from "../Adapters/AuthenticationAdapter"
import { Seat } from "../Adapters/SeatsAdapter"
import { Stack } from "@mui/material"

export type SeatMenuProps = {
  seat: Seat
  onReserve: (seat: Seat) => void
  onRemove: (seat: Seat) => void
  onReserveFor: (seat: Seat) => void
  onRemoveFor: (seat: Seat) => void
  onMoveFor: (fromSeat: Seat) => void
}

export function SeatMenu(props: SeatMenuProps & MenuProps) {
  const { loggedInUser } = useAuthenticationAdapter()

  const makeReservationFor = () => {
    return (
      <MenuItem onClick={() => props.onReserveFor(props.seat)}>
        <ListItemIcon>
          <AddLink fontSize="small" />
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const moveReservationFor = () => {
    return (
      <MenuItem onClick={() => props.onMoveFor(props.seat)}>
        <ListItemIcon>
          <Shuffle fontSize="small" />
        </ListItemIcon>
        <ListItemText>Flytt reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const removeReservationFor = () => {
    return (
      <MenuItem onClick={() => props.onRemoveFor(props.seat)}>
        <ListItemIcon>
          <Delete fontSize="small" />
        </ListItemIcon>
        <ListItemText>Fjern reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const makeReservation = () => {
    return (
      <MenuItem onClick={() => props.onReserve(props.seat)}>
        <ListItemIcon>
          <AddCircleOutline fontSize="small" />
        </ListItemIcon>
        <ListItemText>Reserver denne plassen</ListItemText>
      </MenuItem>
    )
  }

  const removeReservation = () => {
    return (
      <MenuItem onClick={() => props.onRemove(props.seat)}>
        <ListItemIcon>
          <ListItemIcon>
            <Delete fontSize="small" />
          </ListItemIcon>
        </ListItemIcon>
        <ListItemText>Fjern din reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const header = (text: string) => (
    <Divider flexItem sx={{ width: "100%" }} orientation={"horizontal"}>
      <Typography color="text.secondary" variant={"subtitle2"}>
        {text}
      </Typography>
    </Divider>
  )

  const message = (text: string) => (
    <MenuItem
      disabled
      sx={{
        "&.Mui-disabled": {
          opacity: 1,
        },
      }}
    >
      {text}
    </MenuItem>
  )

  const reservedByHeader = (reservedBy: User) => {
    return (
      <Stack
        direction={"row"}
        justifyContent={"flex-start"}
        alignItems="stretch"
        maxWidth={"100%"}
        minWidth={0}
        sx={{ cursor: "default", padding: "0.5em" }}
      >
        <ListItemIcon>
          <DiscordUserAvatar user={reservedBy} />
        </ListItemIcon>
        <Stack>
          <Typography variant="caption" noWrap color={"text.secondary"}>
            Resevert av
          </Typography>
          <Stack direction="row" sx={{ minWidth: 0, maxWidth: "10em" }}>
            <Typography sx={{ fontWeight: "medium" }} noWrap>
              {reservedBy.name}
              aaaaaaaaaaaaaaaaaaasdasdsdkjlhsduiweuhikhuiahuifsdhuasfiduuiyohs
            </Typography>
          </Stack>
        </Stack>
      </Stack>
    )
  }

  const getMenuComponents = () => {
    const components: JSX.Element[] = []
    if (props.seat.reservedBy != null) {
      components.push(reservedByHeader(props.seat.reservedBy))
    }

    if (loggedInUser == null) {
      if (props.seat.reservedBy == null) {
        components.push(message("Logg inn for å reservere plass"))
      }
    } else if (loggedInUser != null) {
      if (loggedInUser.isInRole(Role.OPERATOR)) {
        if (props.seat.reservedBy != null) {
          components.push(
            header("ADMIN"),
            removeReservationFor(),
            moveReservationFor()
          )
        } else {
          components.push(header("ADMIN"), makeReservationFor())
        }
      }

      if (
        props.seat.reservedBy != null &&
        props.seat.reservedBy.id == loggedInUser.id
      ) {
        if (components.length > 0) components.push(header("VALG"))
        components.push(removeReservation())
      } else if (props.seat.reservedBy == null) {
        if (components.length > 0) components.push(header("VALG"))
        components.push(makeReservation())
      }
    }
    return components
  }

  return (
    <Menu sx={{ width: 320 }} {...props}>
      {getMenuComponents()}
    </Menu>
  )
}
