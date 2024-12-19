import { AddCircleOutline, AddLink, Delete, Shuffle } from "@mui/icons-material"
import {
  Autocomplete,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Container,
  Divider,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  MenuProps,
  Stack,
  TextField,
  Typography,
} from "@mui/material"
import * as React from "react"
import { useState } from "react"
import { useAuth } from "../Adapters/AuthAdapter"
import { useGuildUsers } from "../Adapters/Guilds/GuildUsers"
import { Lan, Role, Seat, User } from "../Adapters/Models"
import useReservationAdapter from "../Adapters/Reservation"
import { useSeats } from "../Adapters/Seats"
import { DiscordUserAvatar } from "../Components/DiscordAvatar"
import { useAlerts } from "../Contexts/AlertContext"
import { useDialogs } from "../Contexts/DialogContext"
import { useActiveGuild } from "../Adapters/ActiveGuild"
import createSeats from "../StaticSeats"

export default function Seats() {
  const activeGuild = useActiveGuild()

  if (!activeGuild || !activeGuild.lan) {
    return <ErrorMessage message="Det er ikke konfigurert et aktivt lan" />
  } else {
    return <SeatsWithLan guildId={activeGuild.guildId} activeLan={activeGuild.lan} />
  }
}

function ErrorMessage(props: { message: string }) {
  return <Stack width="100%" justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }}>
    <Typography>{props.message}</Typography>
  </Stack>
}

type AwaitingSelectSeat = {
  onSelected: (seat: Seat) => Promise<void>
  seat: Seat
  tooltip: string
}

function SeatsWithLan(props: { guildId: string, activeLan: Lan }) {
  const { alertSuccess, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { loggedInUser } = useAuth()
  const { reservedSeat, seats, createNewSeat } = useSeats(props.guildId, props.activeLan)
  const reservationAdapter = useReservationAdapter(props.guildId, props.activeLan)
  const [awaitingSelectSeat, setAwaitingSelectSeat] = useState<AwaitingSelectSeat | null>(null)
  
  async function appyStaticSeats() {
    const seats = createSeats()
    for (const seat of seats) {
      createNewSeat(seat)
    }
  }
  
  async function handleReserve(toSeat: Seat) {
    if (reservedSeat != null) {
      const fromSeat = reservedSeat
      const result = await showDialog(
        "Endre sete",
        `Sikker på at du vil endre sete fra ${fromSeat.title} til ${toSeat.title}?`,
        toSeat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        const toSeat = result.metadata

        await alertLoading("Flytter reservasjon...", async () => {
          await reservationAdapter.moveReservation(fromSeat, toSeat)
        })

        alertSuccess(
          `Du har flyttet din reservasjon fra plass ${fromSeat.title} til plass ${toSeat.title}`
        )
      }
    } else {
      await alertLoading("Reserverer...", async () => {
        await reservationAdapter.makeReservation(toSeat)
      })

      alertSuccess("Du har reservert plass " + toSeat.title)
    }
  }

  async function handleRemove(seat: Seat) {
    const result = await showDialog(
      "Fjern reservasjon",
      "Sikker på at du vil gi fra deg denne plassen?",
      seat,
      "Ja",
      "Nei"
    )

    if (result.positive) {
      await alertLoading("Sletter reservasjon...", async () => {
        await reservationAdapter.deleteReservation(result.metadata)
      })

      alertSuccess("Du har slettet din reservasjon")
    }
  }

  async function handleRemoveFor(seat: Seat) {
    if (seat.reservedBy) {
      const result = await showDialog(
        "Fjern reservasjon",
        `Sikker på at du vil fjerne ${seat.reservedBy.name} fra plass ${seat.title}?`,
        seat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        await alertLoading("Fjerner reservasjon...", async () => {
          await reservationAdapter.deleteReservationFor(result.metadata)
        })

        alertSuccess(`Du har fjernet ${seat.reservedBy.name} sin reservasjon`)
      }
    }
  }

  async function handleReserveFor(seat: Seat, user: User) {
    await alertLoading(`Reserverer plass ${seat.title} for ${user.name} ...`, async () => {
      await reservationAdapter.makeReservationFor(seat, user)
    })
    alertSuccess(`Plass ${seat.title} ble reservert for ${user.name}`)
  }

  async function handleSeatSelectedForMove(fromSeat: Seat, toSeat: Seat) {
    const result = await showDialog(
      "Endre sete",
      `Sikker på at du vil flytte ${fromSeat.reservedBy?.name} fra plass ${fromSeat.title} til plass ${toSeat.title}?`,
      toSeat,
      "Ja",
      "Nei"
    )

    if (result.positive) {
      const toSeat = result.metadata

      await alertLoading("Flytter reservasjon...", async () => {
        await reservationAdapter.moveReservationFor(fromSeat, toSeat)
      })

      alertSuccess(
        `Du har flyttet ${fromSeat.reservedBy?.name} fra plass ${fromSeat.title} til plass ${toSeat.title}`
      )
    }
  }

  function handleMoveFor(fromSeat: Seat) {
    setAwaitingSelectSeat({
      seat: fromSeat,
      tooltip: `Velg hvilken plass du vil flytte ${fromSeat.reservedBy?.name} til`,
      onSelected: async (toSeat: Seat) => {
        await handleSeatSelectedForMove(fromSeat, toSeat)
      },
    })
  }

  async function handleSeatClick(seat: Seat) {
    if (awaitingSelectSeat) {
      await awaitingSelectSeat.onSelected(seat)
      setAwaitingSelectSeat(null)
    }
  }

  function handleCancelMoveFor() {
    setAwaitingSelectSeat(null)
  }

  function getUserDisabledForReservation(user: User) {
    return Boolean(seats.find(seat => seat.reservedBy?.id == user.id))
  }

  const getSeatColor = (seat: Seat): string => {
    if (seat.reservedBy != null && loggedInUser != null && seat.reservedBy.id === loggedInUser.id) {
      return "#0f3f6a"
    } else if (seat.reservedBy != null) {
      return "#aa3030"
    } else {
      return "#0f6a0f"
    }
  }

  return <Stack spacing={2} sx={{ my: 2, alignItems: "center" }}>
    {awaitingSelectSeat && awaitingSelectSeat.seat.reservedBy && <AwaitingSelectInfoCard
        userName={awaitingSelectSeat.seat.reservedBy.name}
        seatTitle={awaitingSelectSeat.seat.title}
        onCancelClick={handleCancelMoveFor}
      />}
    <Box sx={{ flexGrow: 1 }}>
      <Box
        sx={{
          display: "flex",
          width: "100%",
          height: "calc(100% - 64px)",
          overflowX: "hidden",
        }}
      >
        <Box
          sx={{
            marginBottom: "auto",
            marginLeft: "auto",
            marginRight: "auto",
            position: "relative",
          }}
        >
          <img
            src={`data:image/svg+xml;base64,${props.activeLan.background}`}
            alt=""
            style={{
              width: "100%",
            }}
          />

          {seats.map(seat => <SeatButton
            key={seat.id}
            seat={seat}
            guildId={props.guildId}
            getUserDisabled={getUserDisabledForReservation}
            color={getSeatColor(seat)}
            onSeatClick={awaitingSelectSeat ? handleSeatClick : undefined}
            onReserve={handleReserve}
            onRemove={handleRemove}
            onReserveFor={handleReserveFor}
            onRemoveFor={handleRemoveFor}
            onMoveFor={handleMoveFor}
          />)}
        </Box>
      </Box>
    </Box>
    {loggedInUser && loggedInUser.roles.includes(Role.SUPERADMIN) ? <Button variant={"outlined"} onClick={appyStaticSeats}>Apply static seats</Button> : <></>}
  </Stack>
}

type SeatButtonProps = {
  color: string
  onSeatClick?: (seat: Seat) => void
} & SeatMenuProps

function AwaitingSelectInfoCard(props: {
  userName: string
  seatTitle: string
  onCancelClick: () => void
}) {
  return <Card>
    <CardContent>
      <Typography color="text.secondary" gutterBottom>
        Du flytter på {props.userName} fra plass {props.seatTitle}
      </Typography>
      <Typography>
        Trykk på en plass for å velge hvor du vil flytte {props.userName} sin reservasjon til,
        eller trykk avbryt for å avbryte.
      </Typography>
    </CardContent>
    <CardActions>
      <Button onClick={props.onCancelClick} size="small">
        Avbryt
      </Button>
    </CardActions>
  </Card>
}

function SeatButton(props: SeatButtonProps) {
  const [menuAnchor, setMenuAnchor] = React.useState<null | HTMLElement>(null)
  const menuOpen = Boolean(menuAnchor)

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    if (props.onSeatClick != undefined) {
      props.onSeatClick(props.seat)
    } else {
      setMenuAnchor(event.currentTarget)
    }
  }

  const handleClose = () => {
    setMenuAnchor(null)
  }

  function handleRemove(seat: Seat) {
    props.onRemove(seat)
    handleClose()
  }

  function handleRemoveFor(seat: Seat) {
    props.onRemoveFor(seat)
    handleClose()
  }

  function handleMoveFor(seat: Seat) {
    props.onMoveFor(seat)
    handleClose()
  }

  const renderSeat = (seat: Seat) => {
    return <Container>
      <Box
        key={seat.id}
        onClick={handleClick}
        sx={{
          position: "absolute",
          minWidth: "0",
          top: seat.bounds.y + "%",
          left: seat.bounds.x + "%",
          width: seat.bounds.width + "%",
          height: seat.bounds.height + "%",
          border: "1px #ffffff61 solid",
          backgroundColor: props.color,
          cursor: "pointer",
          textAlign: "center",
          display: "flex",
        }}
      >
        <Typography
          variant="subtitle1"
          gutterBottom
          component="p"
          sx={{ lineHeight: 1, fontSize: "0.9rem", margin: "auto" }}
        >
          {seat.title}
        </Typography>
      </Box>
      {menuOpen && <SeatMenu
          {...props}
          onRemove={handleRemove}
          onRemoveFor={handleRemoveFor}
          onMoveFor={handleMoveFor}
          seat={seat}
          menuProps={{
            anchorEl: menuAnchor,
            onClose: handleClose,
            open: menuOpen,
            anchorOrigin: {
              vertical: "top",
              horizontal: "center",
            },
            transformOrigin: {
              vertical: "bottom",
              horizontal: "center",
            },
          }}
        />}
    </Container>
  }

  return renderSeat(props.seat)
}

function GuildUsersAutoComplete(props: {
  guildId: string
  onUserSelected: (user: User | null) => void
  getOptionDisabled?: (user: User) => boolean
}) {
  const guildUsers = useGuildUsers(props.guildId)
  return <Autocomplete
    sx={{ width: "100%" }}
    size="small"
    autoHighlight
    disableClearable
    options={guildUsers}
    getOptionDisabled={props.getOptionDisabled}
    getOptionLabel={option => option.name}
    onChange={(e, value) => props.onUserSelected(value)}
    renderOption={(props, option) => <Box component="li" sx={{ "& > img": { mr: 2, flexShrink: 0 } }} {...props}>
      <DiscordUserAvatar user={option} sx={{ marginRight: "0.5em" }} />
      <Typography noWrap>{option.name}</Typography>
    </Box>}
    renderInput={params => <TextField {...params} label="User" />}
  />
}

type SeatMenuProps = {
  seat: Seat
  guildId: string
  getUserDisabled: (user: User) => boolean
  onReserve: (seat: Seat) => void
  onRemove: (seat: Seat) => void
  onReserveFor: (seat: Seat, user: User) => void
  onRemoveFor: (seat: Seat) => void
  onMoveFor: (fromSeat: Seat) => void
}

function SeatMenu(props: SeatMenuProps & { menuProps: MenuProps }) {
  const { loggedInUser } = useAuth()
  const [showUserList, setShowUserList] = useState<boolean>(false)

  function handleMakeReservationForClick() {
    setShowUserList(true)
  }

  function handleuserSelectedForReservation(user: User | null) {
    setShowUserList(false)
    if (user != null) {
      props.onReserveFor(props.seat, user)
    }
  }

  const usersList = () => {
    return <MenuItem>
      <GuildUsersAutoComplete
        guildId={props.guildId}
        onUserSelected={handleuserSelectedForReservation}
        getOptionDisabled={props.getUserDisabled}
      />
    </MenuItem>
  }

  const makeReservationFor = () => {
    return <MenuItem onClick={() => handleMakeReservationForClick()}>
      <ListItemIcon>
        <AddLink fontSize="small" />
      </ListItemIcon>
      <ListItemText>Legg til reservasjon</ListItemText>
    </MenuItem>
  }

  const moveReservationFor = () => {
    return <MenuItem onClick={() => props.onMoveFor(props.seat)}>
      <ListItemIcon>
        <Shuffle fontSize="small" />
      </ListItemIcon>
      <ListItemText>Flytt reservasjon</ListItemText>
    </MenuItem>
  }

  const removeReservationFor = () => {
    return <MenuItem onClick={() => props.onRemoveFor(props.seat)}>
      <ListItemIcon>
        <Delete fontSize="small" />
      </ListItemIcon>
      <ListItemText>Fjern reservasjon</ListItemText>
    </MenuItem>
  }

  const makeReservation = () => {
    return <MenuItem onClick={() => props.onReserve(props.seat)}>
      <ListItemIcon>
        <AddCircleOutline fontSize="small" />
      </ListItemIcon>
      <ListItemText>Reserver denne plassen</ListItemText>
    </MenuItem>
  }

  const removeReservation = () => {
    return <MenuItem onClick={() => props.onRemove(props.seat)}>
      <ListItemIcon>
        <ListItemIcon>
          <Delete fontSize="small" />
        </ListItemIcon>
      </ListItemIcon>
      <ListItemText>Fjern din reservasjon</ListItemText>
    </MenuItem>
  }

  const header = (text: string) => <Divider flexItem sx={{ width: "100%" }} orientation={"horizontal"}>
    <Typography color="text.secondary" variant={"subtitle2"}>
      {text}
    </Typography>
  </Divider>

  const message = (text: string) => <MenuItem
    disabled
    sx={{
      "&.Mui-disabled": {
        opacity: 1,
      },
    }}
  >
    {text}
  </MenuItem>

  const reservedByHeader = (reservedBy: User) => {
    return <Stack
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
          </Typography>
        </Stack>
      </Stack>
    </Stack>
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
      if (loggedInUser.roles.includes(Role.OPERATOR)) {
        if (props.seat.reservedBy != null) {
          components.push(header("ADMIN"), removeReservationFor(), moveReservationFor())
        } else if (showUserList) {
          components.push(header("ADMIN"), usersList())
        } else {
          components.push(header("ADMIN"), makeReservationFor())
        }
      }

      if (props.seat.reservedBy != null && props.seat.reservedBy.id == loggedInUser.id) {
        if (components.length > 0) components.push(header("VALG"))
        components.push(removeReservation())
      } else if (props.seat.reservedBy == null) {
        if (components.length > 0) components.push(header("VALG"))
        components.push(makeReservation())
      }
    }
    return components
  }

  return <Menu sx={{ width: 320 }} {...props.menuProps}>
    {getMenuComponents()}
  </Menu>
}
