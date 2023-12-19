import { Create } from "@refinedev/mui";
import {
  Box,
  TextField,
  Checkbox,
  FormControlLabel,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from "@mui/material";
import { useForm } from "@refinedev/react-hook-form";
import { IResourceComponentsProps, useTranslate } from "@refinedev/core";
import { Controller } from "react-hook-form";

export const SecretKeyCreate: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const {
    saveButtonProps,
    refineCore: { formLoading },
    register,
    control,
    formState: { errors },
  } = useForm();

  return (
    <Create isLoading={formLoading} saveButtonProps={saveButtonProps}>
      <Box
        component="form"
        sx={{ display: "flex", flexDirection: "column" }}
        autoComplete="off"
      >
        <FormControl>
          <InputLabel>{translate("secret-keys.fields.keyType")}</InputLabel>
          <Select
            {...register("keyType", {
              required: "This field is required",
              valueAsNumber: true,
            })}
            error={!!(errors as any)?.keyType}
            fullWidth
            type="number"
            name="keyType"
          >
            <MenuItem value={1}>Publisher</MenuItem>
            <MenuItem value={2}>Subscriber</MenuItem>
            <MenuItem value={3}>Publisher & Subscriber</MenuItem>
          </Select>
        </FormControl>
        <TextField
          {...register("key")}
          error={!!(errors as any)?.key}
          helperText={(errors as any)?.key?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("secret-keys.fields.key")}
          name="key"
        />
        <Controller
          control={control}
          name="isApproved"
          // eslint-disable-next-line
          defaultValue={null as any}
          render={({ field }) => (
            <FormControlLabel
              label={translate("secret-keys.fields.isApproved")}
              control={
                <Checkbox
                  {...field}
                  checked={field.value}
                  onChange={(event) => {
                    field.onChange(event.target.checked);
                  }}
                />
              }
            />
          )}
        />
      </Box>
    </Create>
  );
};
