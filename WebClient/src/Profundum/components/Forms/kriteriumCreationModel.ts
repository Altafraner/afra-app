export interface KriteriumCreationModel {
    label: string | undefined;
    fachbereiche: string[] | undefined;
    variant: 'create' | 'update';
    isFachlich: boolean | undefined;
}
