export interface KriteriumCreationModel {
    label: string | undefined;
    categories: string[] | undefined;
    variant: 'create' | 'update';
}
