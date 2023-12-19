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
import {
  HttpError,
  IResourceComponentsProps,
  useTranslate,
} from "@refinedev/core";
import { Controller } from "react-hook-form";
import { ISecretKey, Nullable } from "../../interfaces";

export const SecretKeyCreate: React.FC<IResourceComponentsProps> = () => {
  const translate = useTranslate();
  const {
    saveButtonProps,
    refineCore: { formLoading },
    register,
    control,
    formState: { errors },
  } = useForm<ISecretKey, HttpError, Nullable<ISecretKey>>();

  return (
    <Create isLoading={formLoading} saveButtonProps={saveButtonProps}>
      <Box
        component="form"
        sx={{ display: "flex", flexDirection: "column" }}
        autoComplete="off"
      >
        <FormControl>
          <InputLabel id="keyType">
            {translate("secret-keys.fields.keyType")}
          </InputLabel>
          <Select
            {...register("keyType", {
              required: "This field is required",
              valueAsNumber: true,
            })}
            error={!!errors?.keyType}
            fullWidth
            type="number"
            name="keyType"
            labelId="keyType"
          >
            <MenuItem value={1}>Publisher</MenuItem>
            <MenuItem value={2}>Subscriber</MenuItem>
            <MenuItem value={3}>Publisher & Subscriber</MenuItem>
          </Select>
        </FormControl>
        <TextField
          {...register("key")}
          error={!!errors?.key}
          helperText={errors?.key?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("secret-keys.fields.key")}
          name="key"
        />
        <TextField
          {...register("description")}
          error={!!errors?.description}
          helperText={errors?.description?.message}
          margin="normal"
          fullWidth
          InputLabelProps={{ shrink: true }}
          type="text"
          label={translate("secret-keys.fields.description")}
          name="description"
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
                  checked={!!field.value}
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
