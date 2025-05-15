import React from 'react';
import { 
  FormControl, 
  InputLabel, 
  Select, 
  MenuItem, 
  FormHelperText,
  Stack,
  Checkbox,
  Box,
  Chip,
  ListItemText,
  TextField
} from '@mui/material';
import { eventTypes } from '../models/formData';

interface EventSelectorProps {
  triggerEvents: string[];
  setTriggerEvents: (events: string[]) => void;
  murderTypeFilter: string;
  setMurderTypeFilter: (type: string) => void;
}

const EventSelector: React.FC<EventSelectorProps> = ({
  triggerEvents,
  setTriggerEvents,
  murderTypeFilter,
  setMurderTypeFilter
}) => {
  const handleEventChange = (event: any) => {
    const value = event.target.value as string[];
    setTriggerEvents(value);
  };

  const handleDeleteEvent = (eventToDelete: string) => () => {
    setTriggerEvents(triggerEvents.filter(event => event !== eventToDelete));
  };

  return (
    <Stack spacing={2}>
      <FormControl fullWidth>
        <InputLabel id="trigger-events-label">Trigger Events</InputLabel>
        <Select
          labelId="trigger-events-label"
          id="trigger-events"
          multiple
          value={triggerEvents}
          label="Trigger Events"
          onChange={handleEventChange}
          renderValue={(selected) => (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {(selected as string[]).map((value) => {
                const eventType = eventTypes.find(et => et.value === value);
                return (
                  <Chip 
                    key={value} 
                    label={eventType?.label || value} 
                    onDelete={handleDeleteEvent(value)}
                    onMouseDown={(event) => {
                      event.stopPropagation();
                    }}
                  />
                );
              })}
            </Box>
          )}
        >
          {eventTypes.map((event) => (
            <MenuItem key={event.value} value={event.value}>
              <Checkbox checked={triggerEvents.indexOf(event.value) > -1} />
              <ListItemText primary={event.label} />
            </MenuItem>
          ))}
        </Select>
        <FormHelperText>
          Select the events that will trigger this item to spawn
        </FormHelperText>
      </FormControl>

      <TextField
        fullWidth
        label="Murder Type"
        value={murderTypeFilter}
        onChange={(e) => setMurderTypeFilter(e.target.value)}
        helperText="Enter the murder type (e.g., TheDoveKiller, TheScammer)"
        variant="outlined"
      />
    </Stack>
  );
};

export default EventSelector;
