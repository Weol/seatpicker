import * as React from 'react';
import Divider from '@mui/material/Divider';
import MenuItem from '@mui/material/MenuItem';
import ListItemText from '@mui/material/ListItemText';
import ListItemIcon from '@mui/material/ListItemIcon';
import Seat from "../Models/Seat";
import Menu, {MenuProps} from "@mui/material/Menu";
import DiscordAvatar from "./DiscordAvatar";
import useLoggedInUser from "../LoggedInUserHook";
import {Role} from "../Models/Role";
import {Add, ContentCut, Delete, Shuffle} from "@mui/icons-material";
import Typography from "@mui/material/Typography";
import {Stack} from "@mui/material";

interface SeatMenuProps {
  seat: Seat
}

export function SeatMenu(props: SeatMenuProps & MenuProps) {
  const loggedInUser = useLoggedInUser()

  const addReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <Add fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const moveReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <Shuffle fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Flytt reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const deleteReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <Delete fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Fjern reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const makeReservation = () => {
    if (props.seat.reservedBy != null) {
      return (
        <MenuItem>
          <ListItemIcon>
            <DiscordAvatar sx={{height: "1em", width: "1em"}} user={props.seat.reservedBy}/>
          </ListItemIcon>
          <ListItemText>Reservert av {props.seat.reservedBy.name}</ListItemText>
        </MenuItem>
      )
    } else {
      return (
        <MenuItem>
          Reserver
        </MenuItem>
      )
    }
  }

  const header = (text: string) => (
    <Divider flexItem sx={{width: "100%"}} orientation={"horizontal"}>
      <Typography color="text.secondary" variant={"subtitle2"}>{text}</Typography>
    </Divider>
  )

  const getMenuComponents = () => {
    if (loggedInUser == null) {
      if (props.seat.reservedBy != null) {
        return [makeReservation()]
      }
    } else {
      if (loggedInUser.roles.includes(Role.OPERATOR)) {
        if (props.seat.reservedBy != null) {
          return [header("ADMIN"), deleteReservation(), moveReservation(), header("USER"), makeReservation()]
        } else {
          return [header("ADMIN"), addReservation(), header("USER"), makeReservation()]
        }
      } else {
        return [makeReservation()]
      }
    }
  }

  return (
    <Menu sx={{width: 320, maxWidth: '100%'}} {...props}>
      {getMenuComponents()}
    </Menu>
  );
}