import * as React from 'react';
import Divider from '@mui/material/Divider';
import Paper from '@mui/material/Paper';
import MenuList from '@mui/material/MenuList';
import MenuItem from '@mui/material/MenuItem';
import ListItemText from '@mui/material/ListItemText';
import ListItemIcon from '@mui/material/ListItemIcon';
import ContentCut from '@mui/icons-material/ContentCut';
import ContentCopy from '@mui/icons-material/ContentCopy';
import ContentPaste from '@mui/icons-material/ContentPaste';
import Cloud from '@mui/icons-material/Cloud';
import Seat from "../Models/Seat";
import {Add, Clear} from "@mui/icons-material";
import Menu, {MenuProps} from "@mui/material/Menu";

interface SeatMenuProps {
  seat: Seat
}

export function SeatMenu(props: SeatMenuProps & MenuProps) {
  const addReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const moveReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const deleteReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const makeReservation = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  const deleteSeat = () => {
    return (
      <MenuItem>
        <ListItemIcon>
          <ContentCut fontSize="small"/>
        </ListItemIcon>
        <ListItemText>Legg til reservasjon</ListItemText>
      </MenuItem>
    )
  }

  return (
    <Menu sx={{width: 320, maxWidth: '100%', padding: 0}} {...props}>
      <Paper>
        <MenuList dense>
          <MenuItem>
            <ListItemText>Re  sserver</ListItemText>
          </MenuItem>
        </MenuList>
      </Paper>
    </Menu>
  );
}